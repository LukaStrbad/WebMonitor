const siPrefixes = ['', 'k', 'M', 'G', 'T', 'P', 'E', 'Z', 'Y', 'R', 'Q'];

export class MemoryByteOptions {
  private _isBase2: boolean;
  private _outputBits: boolean;
  private _outputPrecision: number;

  constructor(
    isBase2: boolean = true,
    isBits: boolean = false,
    outputPrecision = 2
  ) {
    this._isBase2 = isBase2;
    this._outputBits = isBits;
    this._outputPrecision = outputPrecision;
  }

  /**
   * Whether to use base 2 (1024) or base 10 (1000) for prefixes.
   * @example let options = new MemoryByteOptions(true, false);
   * toByteString(1024, options); // 1 KiB
   * options.isBase2 = false;
   * toByteString(1000, options); // 1 kB
   */
  get isBase2(): boolean {
    return this._isBase2;
  }

  set isBase2(value: boolean) {
    this._isBase2 = value;
  }

  /**
   * Whether to convert the output to bits.
   * Input will always be interpreted as bytes.
   * @example let options = new MemoryByteOptions(true, false);
   * toByteString(1024, options); // 1 KiB
   * options.outputBits = true;
   * toByteString(1024, options); // 8 Kib
   */
  get outputBits(): boolean {
    return this._outputBits;
  }

  set outputBits(value: boolean) {
    this._outputBits = value;
  }

  /**
   * The number of decimal places to output.
   */
  get outputPrecision(): number {
    return this._outputPrecision;
  }

  set outputPrecision(value: number) {
    this._outputPrecision = value;
  }

}

export function toByteString(
  value: number | bigint,
  options = new MemoryByteOptions()
) {
  if (value < 0) {
    throw new Error('Value must be greater than or equal to 0.');
  }

  const multiplier = options.isBase2 ? 1024 : 1000;
  const suffix = options.outputBits ? 'b' : 'B';
  let valueNumber = Number(value);

  if (options.outputBits) {
    valueNumber *= 8;
  }

  if (valueNumber < multiplier) {
    return valueNumber + suffix;
  }

  let exp = Math.floor(Math.log(valueNumber) / Math.log(multiplier));
  if (exp >= siPrefixes.length) {
    exp = siPrefixes.length - 1;
  }

  const prefix = siPrefixes[exp];
  const val = valueNumber / Math.pow(multiplier, exp);

  return `${val.toFixed(options.outputPrecision)} ${prefix}${options.isBase2 ? "i" : ""}${suffix}`;
}
