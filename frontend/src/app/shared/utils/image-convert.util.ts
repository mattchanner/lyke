/**
 * Converts a HEIC/HEIF file to JPEG using heic2any.
 * Loaded via dynamic import so the ~300 kB library only enters the bundle
 * when the user actually picks a HEIC image.
 *
 * Returns the original file unchanged for any non-HEIC type.
 */
export async function convertToJpeg(file: File): Promise<File> {
  const heicTypes = ['image/heif', 'image/heic', 'image/heif-sequence', 'image/heic-sequence'];
  const heicExtensions = ['.heif', '.heic'];
  const ext = file.name.toLowerCase().slice(file.name.lastIndexOf('.'));
  const isHeic = heicTypes.includes(file.type.toLowerCase()) || heicExtensions.includes(ext);

  if (!isHeic) return file;

  const { default: heic2any } = await import('heic2any');
  const result = await heic2any({ blob: file, toType: 'image/jpeg', quality: 0.92 });
  const blob = Array.isArray(result) ? result[0] : result;
  const name = file.name.replace(/\.hei[cf](\.\w+)?$/i, '.jpg');
  return new File([blob], name, { type: 'image/jpeg' });
}
