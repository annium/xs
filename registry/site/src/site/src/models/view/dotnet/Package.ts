import { Package as BasePackage } from '../Package'

import { PackageDependency } from './PackageDependency'


export type Package = BasePackage & {
  dependencies: PackageDependency[]
}
