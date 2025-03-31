# Secure File Upload Endpoint

This endpoint handles chunked file uploads with several security measures to help keep your data safe.

- **Dedicated Upload Area:** Files are stored in a dedicated upload directory on a non-system drive.
- **No Execution:** The upload folder is configured to disable execute permissions(done manually not via code).
- **Outside App Directory:** Uploaded files are kept separate from the application files.
- **Safe File Naming:** We generate a unique, safe file name (with a timestamp and session ID) rather than using the raw file name from the user.
- **Extension Whitelisting:** Only approved file types (like `.jpg`, `.png`, `.pdf`, and `.txt`) are allowed.
- **File Size Limits:** Uploads are checked against a maximum size to prevent overly large files.
- **Server-side Checks:** All validations (file type, size, etc.) are enforced on the server—even if client-side checks are bypassed.
- **Virus Scanning:** Files are scanned for malware before being permanently stored.

Note: upload flow can be changed like you see fit but implement those checks accordingly.
This simple, yet robust approach helps ensure that your file uploads are as secure as possible but not immune to attacks , stay stafe.
