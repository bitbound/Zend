import { v4 as uuidv4 } from 'uuid';
import { SavedFile } from "./SavedFile";

export class UploadedFile extends SavedFile {
    constructor(fileName, fileSize, uploadedAt) {
        super();
        this.fileName = fileName;
        this.fileSize = fileSize;
        this.uploadedAt = uploadedAt;
        this.id = uuidv4();
    }

    url;
    percentUploaded;
}