import { chain } from 'lodash'
import React from 'react'
import { NavLink } from 'react-router-dom'

import { DependencyType } from '../../models/view/node/DependencyType'
import { PackageDependency } from '../../models/view/node/PackageDependency'
import { ProjectType } from '../../models/view/ProjectType'
import * as route from '../../utils/route'

import styles from './Dependencies.module.scss'


type Props = {
  dependencies: PackageDependency[]
}

export const Dependencies = ({ dependencies }: Props) => (
  <>
    <div className={styles.header}>Dependencies</div>
    {renderTypeDependencies(dependencies, DependencyType.Normal, 'Base dependencies')}
    {renderTypeDependencies(dependencies, DependencyType.Dev, 'Development dependencies')}
  </>
)


const renderTypeDependencies = (dependencies: Props['dependencies'], type: DependencyType, label: string) => {
  const deps = dependencies.filter(d => d.type === type)

  if (!deps.length)
    return null

  return (
    <div>
      <div className={styles.type}>{label}</div>
      {chain(dependencies).sortBy().map(d => (
        <div className={styles.dependency} key={d.name}>
          <NavLink to={route.pkg(ProjectType.Node, d.name)}>{d.name}</NavLink> ({d.version})
          </div>
      )).value()}
    </div>
  )
}
