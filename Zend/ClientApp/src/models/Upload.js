import { createUuid } from "../utils/rng";

export class Upload {
    constructor(fileName, fileSize, uploadDate) {
        this.fileName = fileName;
        this.fileSize = fileSize;
        this.uploadDate = uploadDate;
        this.id = createUuid();
    }

    id;
    fileName;
    fileSize;
    uploadDate;
    url;
    percentUploaded;
}