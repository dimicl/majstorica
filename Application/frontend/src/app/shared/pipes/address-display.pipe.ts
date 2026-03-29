import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'addressDisplay',
  standalone: true,
})
export class AddressDisplayPipe implements PipeTransform {
  transform(value: unknown): string {
    if (!value) return 'No Address';

    if (typeof value === 'string') {
      return value;
    }

    const addr = value as {
      street?: string | null;
      city?: string | null;
    };

    const street = addr.street?.trim();
    const city = addr.city?.trim();

    if (!street && !city) return 'No Address';

    const parts = [street, city].filter((x) => !!x) as string[];
    console.log(parts);
    return parts.join(', ');
  }
}
