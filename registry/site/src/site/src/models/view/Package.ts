import { Moment } from 'moment'

export type Package = {
  id: string
  name: string
  version: string
  description: string
  published: Moment
  downloads: number
}
