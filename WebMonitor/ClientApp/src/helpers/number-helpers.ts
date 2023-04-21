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

  // In base 2 1kiB = 1024B, in base 10 1kB = 1000B
  /**
   * The multiplier to use for the current base.
   */
  get multiplier(): number {
    return this.isBase2 ? 1024 : 1000;
  }

  /**
   * The suffix to use for the current output type.
   */
  get suffix(): string {
    return this.outputBits ? 'b' : 'B';
  }

}

function getPrefixAndExponent(value: number, multiplier: number): [string, number] {
  let exp = Math.floor(Math.log(value) / Math.log(multiplier));
  if (exp >= siPrefixes.length) {
    exp = siPrefixes.length - 1;
  }

  return [siPrefixes[exp], exp];
}

export function toByteString(
  value: number | bigint,
  options = new MemoryByteOptions()
) {
  if (value < 0) {
    throw new Error('Value must be greater than or equal to 0.');
  }

  const multiplier = options.multiplier;
  const suffix = options.suffix;
  // Convert to number in case value is a bigint
  // We don't need to worry about precision loss because in most cases we're outputting at most 4 digits
  let valueNumber = Number(value);

  if (options.outputBits) {
    valueNumber *= 8;
  }

  if (valueNumber < multiplier) {
    return valueNumber + suffix;
  }

  const [prefix, exp] = getPrefixAndExponent(valueNumber, multiplier);
  const val = valueNumber / Math.pow(multiplier, exp);

  return `${val.toFixed(options.outputPrecision)} ${prefix}${options.isBase2 ? "i" : ""}${suffix}`;
}

export function toByteStringRatio(
  value1: number | bigint,
  value2: number | bigint,
  options = new MemoryByteOptions()
) {
  if (value1 < 0 || value2 < 0) {
    throw new Error('Values must be greater than or equal to 0.');
  }

  const multiplier = options.multiplier;
  const suffix = options.suffix;
  let value1Number = Number(value1);
  let value2Number = Number(value2);

  if (options.outputBits) {
    value1Number *= 8;
    value2Number *= 8;
  }

  const maxValue = Math.max(value1Number, value2Number);

  if (maxValue < multiplier) {
    return `${value1Number} / ${value2Number} ${suffix}`;
  }

  const [prefix, exp] = getPrefixAndExponent(maxValue, multiplier);

  const val1 = value1Number / Math.pow(multiplier, exp);
  const val2 = value2Number / Math.pow(multiplier, exp);

  return `${val1.toFixed(options.outputPrecision)} / ${val2.toFixed(options.outputPrecision)} ${prefix}${options.isBase2 ? "i" : ""}${suffix}`;
}
