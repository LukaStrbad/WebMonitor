export interface FileOrDir {
    type: "file" | "dir";
    path: string;
    basename: string;
    size: bigint | null;
    childrenCount: number | null;
}
