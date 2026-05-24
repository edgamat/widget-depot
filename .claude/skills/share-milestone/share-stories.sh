#!/usr/bin/env bash
# share-stories.sh — Create GitHub issues for all story files in a milestone directory,
# drift-check each one, link them as native sub-issues, and update frontmatter.
#
# Usage: bash share-stories.sh <milestone-dir> <parent-node-id>
#   milestone-dir    path to docs/milestone-NN (absolute or relative to repo root)
#   parent-node-id   GraphQL node ID of the milestone parent issue (I_kwDOR…)
#
# Prerequisites: gh auth status must be valid; python3 must be on PATH.
# Exits 0 on full success, 1 on any failure.

set -euo pipefail

MILESTONE_DIR="${1:?Usage: share-stories.sh <milestone-dir> <parent-node-id>}"
PARENT_NODE_ID="${2:?Usage: share-stories.sh <milestone-dir> <parent-node-id>}"

TMPFILES=()
cleanup() { rm -f "${TMPFILES[@]}" 2>/dev/null || true; }
trap cleanup EXIT

created_issues=()

mapfile -t story_files < <(ls "$MILESTONE_DIR"/*.md | sort)

for STORY_FILE in "${story_files[@]}"; do
    echo ""
    echo "--- Processing: $(basename "$STORY_FILE") ---"

    # 1. Extract title and body (strip YAML frontmatter + H1 line)
    TMPBODY=$(mktemp /tmp/story-body-XXXXXX.md)
    TMPFILES+=("$TMPBODY")

    TITLE=$(python3 - "$STORY_FILE" "$TMPBODY" << 'PYEOF'
import re, sys

story_file, body_file = sys.argv[1], sys.argv[2]

with open(story_file, 'r') as f:
    content = f.read()

fm_pattern = re.compile(r'^---\n.*?^---\n\n?', re.DOTALL | re.MULTILINE)
content_no_fm = fm_pattern.sub('', content, count=1)

lines = content_no_fm.split('\n')
title = lines[0].lstrip('# ').strip()
body = '\n'.join(lines[1:]).lstrip('\n').replace('\r\n', '\n')

with open(body_file, 'w', newline='\n') as f:
    f.write(body)

print(title)
PYEOF
    )

    # 2. Create the GitHub issue
    ISSUE_URL=$(gh issue create --title "$TITLE" --body-file "$TMPBODY" --label story)
    ISSUE_NUMBER=$(echo "$ISSUE_URL" | grep -oE '[0-9]+$')
    echo "Created #$ISSUE_NUMBER: $TITLE"
    echo "  $ISSUE_URL"

    # 3. Drift check — fetch the body back and compare byte-for-byte
    RETURNED_FILE=$(mktemp /tmp/story-returned-XXXXXX.md)
    TMPFILES+=("$RETURNED_FILE")

    gh issue view "$ISSUE_NUMBER" --json body --jq .body > "$RETURNED_FILE"

    DRIFT_STATUS=$(python3 - "$TMPBODY" "$RETURNED_FILE" << 'PYEOF'
import sys, difflib

canonical_path, returned_path = sys.argv[1], sys.argv[2]

with open(canonical_path) as f:
    canonical = f.read().replace('\r\n', '\n').rstrip('\n')
with open(returned_path) as f:
    returned = f.read().replace('\r\n', '\n').rstrip('\n')

if returned == canonical:
    print("MATCH")
else:
    diff = ''.join(difflib.unified_diff(
        canonical.splitlines(keepends=True),
        returned.splitlines(keepends=True),
        fromfile='canonical',
        tofile='github',
    ))
    sys.stdout.write("MISMATCH\n" + diff)
PYEOF
    )

    if [[ "${DRIFT_STATUS%%$'\n'*}" != "MATCH" ]]; then
        echo ""
        echo "ERROR: Drift detected on issue #$ISSUE_NUMBER ($ISSUE_URL)"
        echo "$DRIFT_STATUS"
        echo ""
        if [[ ${#created_issues[@]} -gt 0 ]]; then
            echo "Issues created before this failure: ${created_issues[*]}"
        fi
        echo "Remaining story files still have status: ready and no github_issue."
        exit 1
    fi

    echo "  Drift check: MATCH"

    # 4. Link as a native sub-issue of the milestone parent
    CHILD_NODE_ID=$(gh issue view "$ISSUE_NUMBER" --json id --jq .id)
    gh api graphql \
        -f query='mutation($issueId: ID!, $subIssueId: ID!) { addSubIssue(input: {issueId: $issueId, subIssueId: $subIssueId}) { issue { number } subIssue { number } } }' \
        -f issueId="$PARENT_NODE_ID" \
        -f subIssueId="$CHILD_NODE_ID" > /dev/null
    echo "  Linked #$ISSUE_NUMBER as sub-issue of parent"

    # 5. Update frontmatter in the story file
    python3 - "$STORY_FILE" "$ISSUE_NUMBER" << 'PYEOF'
import re, sys

story_file, issue_number = sys.argv[1], sys.argv[2]

with open(story_file, 'r') as f:
    content = f.read()

content = content.replace('status: ready', 'status: shared', 1)
content = re.sub(r'^(github_issue:)\s*$', f'\\1 {issue_number}', content, flags=re.MULTILINE)

with open(story_file, 'w') as f:
    f.write(content)
PYEOF
    echo "  Frontmatter updated: $(basename "$STORY_FILE")"

    created_issues+=("$ISSUE_NUMBER")
done

echo ""
echo "=== All stories shared successfully ==="
printf "  Created issues: %s\n" "${created_issues[*]}"
