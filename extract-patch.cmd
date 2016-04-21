rem extract patch
git format-patch HEAD~5

rem import patches
git am *.patch