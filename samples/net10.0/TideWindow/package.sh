#!/bin/sh
set -eu
# snippet:package
publish=$1
destination=$2
extension=$3
count=0
for candidate in "$publish/TideWindow.$extension" "$publish/libTideWindow.$extension"; do
    if [ -f "$candidate" ]; then selected=$candidate; count=$((count + 1)); fi
done
[ "$count" -eq 1 ] || { echo 'Expected exactly one TideWindow shared library' >&2; exit 1; }
mkdir -p "$destination"
cp "$selected" "$destination/"
printf '%s\n' "$destination/$(basename "$selected")"
# endsnippet:package
