import _ from 'lodash'
import React from 'react'
import { NavLink } from 'react-router-dom'

import { PackageDependency } from '../../models/view/dotnet/PackageDependency'
import { ProjectType } from '../../models/view/ProjectType'
import * as route from '../../utils/route'

import styles from './Dependencies.module.scss'


type Props = {
  dependencies: PackageDependency[]
}

export function Dependencies({ dependencies }: Props) {
  const frameworks = _.chain(dependencies).map(d => d.framework).uniq().sortBy().value()

  return (
    <>
      <div className={styles.header}>Dependencies</div>
      {frameworks.map(framework => (
        <div key={framework}>
          <div className={styles.framework}>{framework}</div>
          {_.chain(dependencies).filter(d => d.framework === framework).sortBy().map(d => (
            <div className={styles.dependency} key={d.name}>
              <NavLink to={route.pkg(ProjectType.Dotnet, d.name)}>{d.name}</NavLink> ({d.version})
              </div>
          )).value()}
        </div>
      ))}
    </>
  )
}
