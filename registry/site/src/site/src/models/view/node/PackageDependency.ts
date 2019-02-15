import { DependencyType } from './DependencyType'

export default interface PackageDependency {
  type: DependencyType
  name: string
  version: string
}