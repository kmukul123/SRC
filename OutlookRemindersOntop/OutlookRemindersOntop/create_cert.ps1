# This script is specifically tuned for maximum compatibility with PowerShell 5.1.
$fileName = "TestKey.pfx"

$csharpCode = @"
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.IO;

public class CertGenerator {
    public static void CreatePfx(string fileName, string password) {
        using (RSA rsa = RSA.Create(2048)) {
            // Create the request
            var request = new CertificateRequest(
                "CN=OutlookRemindersOntop", 
                rsa, 
                HashAlgorithmName.SHA256, 
                RSASignaturePadding.Pkcs1);

            // Add Code Signing EKU (1.3.6.1.5.5.7.3.3)
            var oids = new OidCollection();
            oids.Add(new Oid("1.3.6.1.5.5.7.3.3"));
            request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(oids, false));

            // Create self-signed certificate (5 years)
            var cert = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5));

            // Export to PFX
            byte[] pfxData = cert.Export(X509ContentType.Pfx, password);
            File.WriteAllBytes(fileName, pfxData);
        }
    }
}
"@

echo "Compiling helper (this may take a second)..."
Add-Type -TypeDefinition $csharpCode -ReferencedAssemblies "System.Security", "System.Core"

echo ""
$pwdRaw = Read-Host -Prompt "Enter the password you want for your New $fileName"
echo ""

echo "Generating $fileName..."
[CertGenerator]::CreatePfx($fileName, $pwdRaw)

echo ""
echo "--------------------------------------------------------"
echo "SUCCESS! Created: $fileName"
echo "Note: This was created in memory and NOT installed on your machine."
echo "--------------------------------------------------------"
echo "You can now run sign.cmd (as a normal user) to sign your app."
