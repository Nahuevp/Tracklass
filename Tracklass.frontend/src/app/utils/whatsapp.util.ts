export function generarWhatsAppLink(telefono?: string, mensaje?: string): string | null {
  if (!telefono || telefono.trim() === '') {
    return null;
  }

  // Quita espacios y cero inicial (asumiendo prefijo 598 de Uruguay)
  const limpio = telefono.replace(/\s+/g, '').replace(/^0/, '');
  let url = `https://wa.me/598${limpio}`;

  if (mensaje) {
    url += `?text=${encodeURIComponent(mensaje)}`;
  }

  return url;
}
