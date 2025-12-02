import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'ellipsis'
})
export class EllipsisPipe implements PipeTransform {

  transform(value: string, maxLength = 20): string {
    // Unicode character for ellipsis
    const ellipsis = "\u2026";
    return value.length > maxLength
      ? value.substring(0, maxLength - 1) + ellipsis // -1 to account for ellipsis
      : value;
  }

}
