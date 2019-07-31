# https://gist.github.com/GottZ/621f4b6994cfb2726e43e18e7a75e49a
# Ark Labs presents - Slack dARK mode
# based on https://gist.github.com/vRobM/bf158904fcbdcd45c94eceaaa58556ec

$slackBaseDir = "$env:LocalAppData\Slack"
$installations = Get-ChildItem $slackBaseDir -Directory | Where-Object { $_.Name.StartsWith("app-") }
$version = $installations | Sort-Object { [version]$_.Name.Substring(4) } | Select-Object -Last 1
Write-Output "Select highest intalled Slack version: $version";

$modAdded = $false;
$customContent = @'
// slack-dARK-mode |-)
document.addEventListener("DOMContentLoaded", async () => {
  try {
    let css = await fetch("https://raw.githubusercontent.com/laCour/slack-night-mode/master/css/raw/black.css");
    if (!css.ok) return;
    css = await css.text();
    // remove all comments. go there to check it: https://regex101.com/r/pvPqAZ/3
    // just removing multiline comments would be fine though. but why worry. // will only apply to URL's and content: "//" then.
    css = css.replace(/\/(?:\/.*$|\*(?:[^]*?\*\/|[^]+))/g, "");
    // checking if malicious code is present and abort style injection.
    // unless you can tell me about a different attack vector,
    // this aborts further execution if it detects presence of possible attack vectors.
    // if you don't have a clue, read this random google result i just found:
    // https://www.netsparker.com/blog/web-security/private-data-stolen-exploiting-css-injection/
    if (/url\s*\(/u.test(css) || /@\s*import/u.test(css)) return;
    var style = document.createElement("style");
    // using textContent will extinguish any html injections
    style.textContent = css;
    document.head.appendChild(style);
  } catch (e) {
    const fs = require("fs");
    const path = require("path");
    const os = require("os");
    fs.writeFileSync(path.join(os.tmpdir(), "slackfail.log"), e.toString());
  }
});
'@

if ((Get-Content "$($version.FullName)\resources\app.asar.unpacked\src\static\index.js" | %{$_ -match "// slack-dARK-mode |-)"}) -notcontains $true) {
    Add-Content "$($version.FullName)\resources\app.asar.unpacked\src\static\index.js" $customContent
    Write-Host "Mod Added To index.js";
    $modAdded = $true;
} else {
    Write-Host "Mod Detected In index.js - Skipping";
}

if ((Get-Content "$($version.FullName)\resources\app.asar.unpacked\src\static\ssb-interop.js" | %{$_ -match "// slack-dARK-mode |-)"}) -notcontains $true) {
    Add-Content "$($version.FullName)\resources\app.asar.unpacked\src\static\ssb-interop.js" $customContent
    Write-Host "Mod Added To ssb-interop.js";
    $modAdded = $true;
} else {
    Write-Host "Mod Detected In ssb-interop.js - Skipping";
}

if ($modAdded -eq $true) {
    if((Get-Process "slack" -ErrorAction SilentlyContinue) -ne $null) {
        Write-Host "Mod Complete - Mod Will Take Effect After Slack Is Restarted";
    } else {
        Write-Host "Mod Complete";
    }
} else {
    Write-Host "Mod Already Active - No Further Action Is Needed.";
}