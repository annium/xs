import moment from 'moment'

import Package from '../view/Package'


export default interface PackageData {
  id: string
  name: string
  version: string
  description: string
  published: string
  downloads: number
}

export const toPackage = (data: PackageData): Package => {
  return {
    ...data,
    published: moment(data.published),
  }
}