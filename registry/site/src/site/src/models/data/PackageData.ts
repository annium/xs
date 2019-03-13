import moment from 'moment'

import { Package } from '../view/Package'


export type PackageData = {
  id: string
  name: string
  version: string
  description: string
  published: string
  downloads: number
}

export const toPackage = (data: PackageData): Package => ({
  ...data,
  published: moment(data.published),
})
