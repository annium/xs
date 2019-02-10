import { Moment } from 'moment'

export default interface Package {
  id: string
  name: string
  version: string
  description: string
  published: Moment
  downloads: number
}