import moment from 'moment'

import MetaPackage from '../view/MetaPackage'
import MetaPackagePermission from '../view/MetaPackagePermission'
import { ProjectType } from '../view/ProjectType'


export default interface MetaPackageData {
  id: string,
  type: string
  name: string
  version: string
  description: string
  published: string
  downloads: number
  ownerId: string
  owner: string
  permissions: MetaPackagePermission[]
}

export const toMetaPackage = (data: MetaPackageData): MetaPackage => {
  if (!Object.values(ProjectType).includes(data.type))
    throw new Error(`Project type ${data.type} is not supported`)

  const type = data.type as ProjectType
  return {
    ...data,
    type,
    published: moment(data.published),
  }
}