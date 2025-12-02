export function replaceValues<T>(target: T, source: T): void {
    for (const key in target) {
        target[key] = source[key];
    }
}
