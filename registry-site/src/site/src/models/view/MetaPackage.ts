import { Moment } from 'moment'

import MetaPackagePermission from './MetaPackagePermission'

export default interface MetaPackage {
  id: string,
  type: string,
  name: string,
  version: string,
  description: string,
  published: Moment,
  downloads: number,
  ownerId: string,
  owner: string,
  permissions: MetaPackagePermission[]
}