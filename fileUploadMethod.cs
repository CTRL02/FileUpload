public async Task<IActionResult> UploadChunk(IFormFile file, [FromForm] int resumableChunkNumber, [FromForm] int resumableTotalChunks)
{
    var uploadPath = _configuration["UploadSettings:UploadPath"];

    // Ensure the upload path is not inside the application's directory tree
    var appBasePath = AppDomain.CurrentDomain.BaseDirectory;
    if (Path.GetFullPath(uploadPath).StartsWith(Path.GetFullPath(appBasePath), StringComparison.OrdinalIgnoreCase))
    {
        return BadRequest("Upload path must not be within the application directory.");
    }

    // Check the file size do not exceed requirments you can remove it in case of dealing with videos or large files in general
    long maxFileSize = Convert.ToInt64(_configuration["UploadSettings:MaxFileSize"]); 
    if (file.Length > maxFileSize)
    {
        return BadRequest("File exceeds maximum allowed size.");
    }

    // Replace VirusScanner.ScanAsync with your actual scanning implementation or use os/eval to use external anitivirus
    if (!await VirusScanner.ScanAsync(file))
    {
        return BadRequest("File failed virus scan.");
    }

    // Retrieve or generate the upload session ID 
    var uploadSessionId = Request.Headers["Upload-Session-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString();
    Response.Headers.Add("Upload-Session-Id", uploadSessionId);

    // here i am logging user input from file name to extension maybe do some checks here if you want and flag user if input is mal.
    var originalFileName = Path.GetFileName(file.FileName);
    var fileExtension = Path.GetExtension(originalFileName).ToLowerInvariant();

    // make sure extensions are valid and malicious and update them as needed
    var allowedExtensions = new[] { ".jpg", ".png", ".pdf", ".txt" , ".mp4" }; 
    if (!allowedExtensions.Contains(fileExtension))
    {
        return BadRequest("File type is not allowed.");
    }

    // here we ignore file name provided by user 
    var safeFileName = $"{uploadSessionId}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{fileExtension}";

    Directory.CreateDirectory(uploadPath);

    var chunkFileName = $"{safeFileName}.part{resumableChunkNumber}";
    var chunkFilePath = Path.Combine(uploadPath, chunkFileName);
    using (var stream = new FileStream(chunkFilePath, FileMode.CreateNew))
    {
        await file.CopyToAsync(stream);
    }

    var chunkFiles = Directory.GetFiles(uploadPath, $"{safeFileName}.part*");
    if (chunkFiles.Length == resumableTotalChunks)
    {
        var finalFilePath = Path.Combine(uploadPath, safeFileName);

        // Prevent overwriting an existing file.
        if (System.IO.File.Exists(finalFilePath))
        {
            safeFileName = $"{uploadSessionId}_{DateTime.UtcNow:yyyyMMddHHmmssfff}_v2{fileExtension}";
            finalFilePath = Path.Combine(uploadPath, safeFileName);
        }

        using (var finalFileStream = new FileStream(finalFilePath, FileMode.CreateNew))
        {
            // Concatenate chunks in order.
            for (var i = 1; i <= resumableTotalChunks; i++)
            {
                var chunkPath = Path.Combine(uploadPath, $"{safeFileName}.part{i}");
                using (var chunkStream = new FileStream(chunkPath, FileMode.Open))
                {
                    await chunkStream.CopyToAsync(finalFileStream);
                }
                System.IO.File.Delete(chunkPath);
            }
        }
    }

    return Ok(new { UploadSessionId = uploadSessionId });
}
