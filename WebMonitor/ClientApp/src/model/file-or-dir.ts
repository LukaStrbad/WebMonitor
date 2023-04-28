export interface FileOrDir {
    type: "file" | "dir";
    path: string;
    basename: string;
    size?: bigint;
    childrenCount?: number;
}
