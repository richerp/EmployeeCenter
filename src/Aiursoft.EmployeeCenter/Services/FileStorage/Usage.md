# File Storage and Upload Module (Refactored)

This module aims to make file uploading **extremely simple and secure**.

The core design philosophy is **"Logical Path"**:

* **Frontend/Database/API**: Only handle clean logical paths (e.g., `avatar/2026/01/14/logo.png`).
* **Backend底层**: Automatically map to physically isolated storage areas (e.g., `/data/Workspace/...`).

---

## 🚫 Prohibited Operations

1. **Prohibited** to use traditional HTML `<input type="file">` controls — this significantly increases workload, greatly expands the attack surface, and prevents access to advanced features like compression and privacy sanitization.
2. **Prohibited** to directly receive `IFormFile` in business Controllers.
3. **Prohibited** to manually concatenate physical paths to access files — always use `StorageService`.

---

## 🔐 Public vs Private Files

This module supports both **public** and **private** file storage:

### Public Files (Default)

* **Storage**: Saved in the `Workspace` folder
* **Access**: Anyone with the URL can download
* **Use Cases**: User avatars, product images, public documents
* **URL Format**: `/download/avatar/2026/01/14/logo.png`

### Private Files (Vault)

* **Storage**: Saved in the isolated `Vault` folder
* **Access**: Requires a time-limited, cryptographically signed token
* **Use Cases**: Private documents, sensitive data, confidential files, user's personal data
* **URL Format**: `/download-private/contract/2026/01/14/agreement.pdf?token=<base64-encoded-token>`
* **Security**: 
  - Tokens expire after 60 minutes
  - HMAC-SHA256 signature prevents tampering
  - Path-specific authorization (token validates exact file path)

---

## 🚀 Quick Integration: Three Steps

### Step 1: View (Place Control)

In the `.cshtml` page, use the `vc:file-upload` component.

**For Public Files:**

```html
<form asp-action="UpdateProfile" method="post">
    <vc:file-upload 
        asp-for="IconPath" 
        upload-endpoint="/upload/avatar" 
        allowed-extensions="jpg png">
    </vc:file-upload>

    <button type="submit">提交</button>
</form>

@* ReSharper disable once Razor.SectionNotResolved *@
@section styles {
    <link rel="stylesheet" href="~/node_modules/dropify/dist/css/dropify.min.css" />
    <link rel="stylesheet" href="~/styles/uploader.css" />
}

@* ReSharper disable once Razor.SectionNotResolved *@
@section scripts {
    <script src="~/node_modules/dropify/dist/js/dropify.min.js"></script>
}
```

**For Private Files:**

```html
<form asp-action="UpdateContract" method="post">
    <vc:file-upload 
        asp-for="ContractPath" 
        upload-endpoint="/upload/contract?useVault=true" 
        allowed-extensions="pdf docx">
    </vc:file-upload>

    <button type="submit">上传私有文件</button>
</form>
```

> **Note**: Add `?useVault=true` to the upload endpoint to store files in the private Vault.

### Step 2: ViewModel (Bind Model and Validation)

The file upload is submitted separately and does not affect the main form submission. The ViewModel only needs a string property to receive the logical path.

Note: The logical path is neither a URL nor a physical path, but a "virtual path" representing the file's location within the storage system. Using a logical path allows the system to automatically handle file storage and access details, and helps prevent attacks exploiting path vulnerabilities.

**For Public Files:**

```csharp
public class UpdateProfileViewModel
{
    [NotNull]
    [Display(Name = "Avatar file")]
    [Required(ErrorMessage = "The avatar file is required.")]
    [MaxLength(150)]
    [MinLength(2)]
    // 这里接收的是逻辑路径，例如 "avatar/2025/01/01/xxx.jpg"
    // ✅ 校验业务桶前缀，确保这里只能提交被上传到 `/avatar` 目录下的文件
    [RegularExpression(@"^avatar.*", ErrorMessage = "请上传正确的头像文件。")]
    public string? IconPath { get; set; }
}
```

**For Private Files:**

```csharp
public class UpdateContractViewModel
{
    [NotNull]
    [Display(Name = "Contract Document")]
    [Required(ErrorMessage = "The contract file is required.")]
    [MaxLength(150)]
    [MinLength(2)]
    // 私有文件同样使用逻辑路径
    // ✅ 校验业务桶前缀，确保只能提交到 `/contract` 目录下
    [RegularExpression(@"^contract.*", ErrorMessage = "请上传正确的合同文件。")]
    public string? ContractPath { get; set; }
}
```

### Step 3: Controller (Save to Database)

The business Controller does not need to handle file streams; treat them like ordinary strings.

```csharp
[HttpPost]
public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel model)
{
    if (!ModelState.IsValid) return this.StackView(model);

    var user = await _userManager.GetUserAsync(User);
    
    // 直接存入逻辑路径
    // DB 存储示例: "avatar/2026/01/14/uuid.jpg"
    user.IconPath = model.IconPath; 
    
    await _userManager.UpdateAsync(user);
    return RedirectToAction(nameof(Index));
}

```

Of course, if the business Controller needs to check files, such as whether they exist or have the correct MIME type, it can also do so through `StorageService`.

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ChangeAvatar(ChangeAvatarViewModel model)
{
    if (!ModelState.IsValid)
    {
        return this.StackView(model);
    }

    // Make sure the file is actually a photo.
    var absolutePath = storageService.GetFilePhysicalPath(model.AvatarUrl);
    if (!await image.IsValidImageAsync(absolutePath))
    {
        ModelState.AddModelError(string.Empty, localizer["The file is not a valid image."]);
        return this.StackView(model);
    }

    // Save the new avatar in the database.
    var user = await GetCurrentUserAsync();
    if (user != null)
    {
        user.AvatarRelativePath = model.AvatarUrl;
        await userManager.UpdateAsync(user);

        // Sign in the user to refresh the avatar.
        await signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction(nameof(Index), new { Message = ManageMessageId.ChangeAvatarSuccess });
    }

    return this.StackView(model);
}
```

### Step 4: Distribute Files

Finally, when we need to display the files, we still use `StorageService` to convert them into internet-accessible URLs.

**For Public Files:**

```html
@inject Aiursoft.EmployeeCenter.Services.FileStorage.StorageService Storage
<img src="@Storage.RelativePathToInternetUrl(Model.IconPath)" alt="User Avatar" />
```

**For Private Files:**

```html
@inject Aiursoft.EmployeeCenter.Services.FileStorage.StorageService Storage
<!-- Token is automatically generated and embedded in the URL -->
<a href="@Storage.RelativePathToInternetUrl(Model.ContractPath, isVault: true)" 
   download="contract.pdf">
    Download Contract
</a>
```

> **Important**: 
> - For private files, set `isVault: true` in `RelativePathToInternetUrl()`
> - The system automatically generates a time-limited token in the URL
> - Tokens are valid for 60 minutes by default
> - Each token is cryptographically signed and path-specific

Even if hackers successfully store malicious files or URLs from third-party servers into the database, they cannot access the actual files during rendering, because the system only allows access to files corresponding to these logical paths through `StorageService`.

**Supported dynamic parameters**:

* `?w=200`: Scale width (automatically maintains aspect ratio).
* `?square=true`: Center-crop to a square.
* **Default behavior**: All image download requests will **automatically remove EXIF information** (GPS, camera settings), protecting user privacy.

## 🏗️ Architecture Deep Dive

This module employs an advanced architecture based on **"physical isolation, logical unification"**.

### 1. Directory Layout

The server's physical disk (`/data`) is strictly divided into four regions:

```text
/data (存储根)
├── Workspace/        # [Source of Truth] 公共原始数据区
│   └── avatar/       # 公共文件：仅供上传写入，不对外直接暴露
│
├── Vault/            # [Private Storage] 私有原始数据区 🔒
│   └── contract/     # 私有文件：需要token才能访问
│
├── ClearExif/        # [Privacy Layer] 隐私清洗区 (缓存)
│   ├── Workspace/    # 公共文件的EXIF清理副本
│   │   └── avatar/
│   └── Vault/        # 私有文件的EXIF清理副本
│       └── contract/
│
└── Compressed/       # [Cache Layer] 缩略图区 (缓存)
    ├── Workspace/    # 公共文件的压缩副本
    │   └── avatar/
    └── Vault/        # 私有文件的压缩副本
        └── contract/
```

### 2. Path Translation Mechanism

`StorageService` acts as a **smart gateway**, mapping user-defined logical paths to different physical regions, achieving "transparent security protection".

**Public Files (Workspace):**

| User Request (API) | Logical Path (Internal) | Actual Physical Operation (Physical) | Notes |
| --- | --- | --- | --- |
| **Upload** | `avatar/img.png` | Write to `/data/Workspace/avatar/img.png` | Original file goes in but never comes out |
| **Download Original** | `avatar/img.png` | Read from `/data/ClearExif/Workspace/avatar/img.png` | Automatically cleans privacy information |
| **Download Thumbnail** | `avatar/img.png?w=200` | Read from `/data/Compressed/Workspace/avatar/img_w200.png` | Automatically compressed for faster delivery |

**Private Files (Vault):**

| User Request (API) | Logical Path (Internal) | Actual Physical Operation (Physical) | Notes |
| --- | --- | --- | --- |
| **Upload** | `contract/doc.pdf` | Write to `/data/Vault/contract/doc.pdf` | Isolated from public storage |
| **Download** | `contract/doc.pdf` | Read from `/data/Vault/contract/doc.pdf` | Requires valid token |
| **Download Image** | `contract/scan.jpg` | Read from `/data/ClearExif/Vault/contract/scan.jpg` | Token + EXIF removal |
| **Download Thumbnail** | `contract/scan.jpg?w=200` | Read from `/data/Compressed/Vault/contract/scan_w200.jpg` | Token + compression |

## 🧩 Core Service Reference

* **StorageService**: Core gateway handling path mapping, file storage, token generation/validation, and access control checks.
* **ImageProcessingService**: Handles image compression, privacy removal, and format validation for both public and private files.
* **FeatureFoldersProvider**: (Internal) Configuration source managing physical directory structure (Workspace, Vault, ClearExif, Compressed).
* **FilesController**: Provides unified upload and download endpoints:
  - `/upload/{subfolder}?useVault=false` - Public file upload
  - `/upload/{subfolder}?useVault=true` - Private file upload
  - `/download/**` - Public file download
  - `/download-private/**?token=xxx` - Private file download with token validation

---

## 🔑 Token-Based Security for Private Files

### How It Works

1. **Token Generation**: When calling `RelativePathToInternetUrl(path, isVault: true)`, the system generates a token using ASP.NET Core's `IDataProtectionProvider`:
   - File path is encrypted
   - Expiry timestamp (60 minutes from generation) is embedded
   - Cryptographic signature prevents tampering

2. **Token Format**: Encrypted, base64-encoded string (handled by DataProtection API)

3. **Token Validation**: On download request, the system automatically verifies:
   - Token hasn't expired
   - Token hasn't been tampered with
   - Requested path matches the authorized path in the token

### Key Management

**No configuration required!** The system uses ASP.NET Core's built-in `DataProtection` API, which automatically:

- Generates and manages encryption keys
- Persists keys securely to disk
- Handles key rotation automatically
- Provides secure key storage across application restarts

> **Note**: In production environments, keys are automatically stored in the application's data directory. For distributed deployments (multiple servers), you may want to configure a shared key storage location using standard DataProtection configuration.

### Programmatic Token Generation

If you need to generate download tokens in backend code:

```csharp
public class DocumentService
{
    private readonly StorageService _storage;
    
    public string GetSecureDownloadUrl(string logicalPath)
    {
        // Generates a time-limited, encrypted URL
        return _storage.RelativePathToInternetUrl(logicalPath, isVault: true);
    }
}
```