/**
 * File information response from the server.
 */
export interface FileInformationResponse {
    name: string;
    path: string;
    size: bigint;
    created: Date | string;
    lastModified: Date | string;
    lastAccessed: Date | string;
}

/**
 * File information with dates converted to Date objects.
 */
export interface FileInformation extends FileInformationResponse {
    created: Date;
    lastModified: Date;
    lastAccessed: Date;
}
