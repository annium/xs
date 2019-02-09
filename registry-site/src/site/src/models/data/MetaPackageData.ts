import moment from 'moment'

import MetaPackage from '../view/MetaPackage'
import MetaPackagePermission from '../view/MetaPackagePermission'


export default interface MetaPackageData {
  id: string,
  type: string,
  name: string,
  version: string,
  description: string,
  published: string,
  downloads: number,
  owner: string,
  permissions: MetaPackagePermission[]
}

export const toMetaPackage = (data: MetaPackageData): MetaPackage => {
  return {
    ...data,
    published: moment(data.published),
  }
}