import { Moment } from 'moment'

import MetaPackagePermission from './MetaPackagePermission'
import { ProjectType } from './ProjectType'

export default interface MetaPackage {
  id: string
  type: ProjectType
  name: string
  version: string
  description: string
  published: Moment
  downloads: number
  ownerId: string
  owner: string
  permissions: MetaPackagePermission[]
}