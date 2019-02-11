export const parseNameVersion = (nameVersion: string): { name: string, version?: string } => {
  if (!nameVersion) throw new Error('Invalid name/version format')

  const parts = nameVersion.split('/')

  // if single part - it's just name
  if (parts.length === 1)
    return { name: nameVersion }

  // handle @-starting special Node.js case
  if (nameVersion.startsWith('@'))
    if (parts.length <= 2) return { name: nameVersion }
    else if (parts.length === 3) return { name: parts.slice(0, -1).join('/'), version: parts[2] }
    else throw new Error('Invalid name/version format')

  // handle normal case
  if (parts.length === 1) return { name: nameVersion }
  else if (parts.length === 2) return { name: parts[0], version: parts[1] }
  else throw new Error('Invalid name/version format')
}