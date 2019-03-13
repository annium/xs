import { DependencyType } from './DependencyType'

export type PackageDependency = {
  type: DependencyType
  name: string
  version: string
}
