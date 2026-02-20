#!/usr/bin/env bash
set -euo pipefail

# Simple release helper:
# - Generates a changelog fragment from commits since last tag
# - Creates annotated git tag and pushes tag

TAG=${1:-}
if [ -z "$TAG" ]; then
  echo "Usage: $0 vX.Y.Z"
  exit 2
fi

LAST_TAG=$(git describe --tags --abbrev=0 2>/dev/null || echo "")
if [ -n "$LAST_TAG" ]; then
  RANGE="$LAST_TAG..HEAD"
else
  RANGE="HEAD"
fi

DATE=$(date -u +%Y-%m-%d)
CHANGELOG_TMP=$(mktemp)

echo "## [$TAG] - $DATE" > "$CHANGELOG_TMP"
echo >> "$CHANGELOG_TMP"
git log --pretty=format:"- %s (%an)" $RANGE >> "$CHANGELOG_TMP"

echo "Generated changelog fragment:"
cat "$CHANGELOG_TMP"

echo "Prepend to CHANGELOG.md and commit? (y/N)"
read -r RESP
if [ "$RESP" = "y" ] || [ "$RESP" = "Y" ]; then
  (echo; cat "$CHANGELOG_TMP"; echo; cat CHANGELOG.md) > CHANGELOG.new && mv CHANGELOG.new CHANGELOG.md
  git add CHANGELOG.md
  git commit -m "chore(release): prepare changelog for $TAG"
fi

git tag -a "$TAG" -m "Release $TAG"
git push origin "$TAG"

rm -f "$CHANGELOG_TMP"
echo "Released $TAG"
