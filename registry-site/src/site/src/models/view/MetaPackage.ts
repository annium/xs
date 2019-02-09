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
  owner: string,
  permissions: MetaPackagePermission[]
}