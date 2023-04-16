export function average(array: number[]): number {
  if (array.length === 0) {
    throw new Error("Array is empty");
  }

  return array.reduce((a, b) => a + b) / array.length;
}
