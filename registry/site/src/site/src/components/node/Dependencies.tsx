import _ from 'lodash'
import React from 'react'
import { NavLink } from 'react-router-dom'

import { DependencyType } from '../../models/view/node/DependencyType'
import PackageDependency from '../../models/view/node/PackageDependency'
import { ProjectType } from '../../models/view/ProjectType'
import route from '../../utils/route'

import styles from './Dependencies.module.scss'


type Props = {
  dependencies: PackageDependency[]
}

export default class Dependencies extends React.PureComponent<Props> {
  render() {
    return (
      <>
        <div className={styles.header}>Dependencies</div>
        {this.renderTypeDependencies(DependencyType.Normal, 'Base dependencies')}
        {this.renderTypeDependencies(DependencyType.Dev, 'Development dependencies')}
      </>
    )
  }

  private renderTypeDependencies(type: DependencyType, label: string) {
    const dependencies = this.props.dependencies.filter(d => d.type === type)

    if (!dependencies.length)
      return null

    return (
      <div>
        <div className={styles.type}>{label}</div>
        {_.chain(dependencies).sortBy().map(d => (
          <div className={styles.dependency} key={d.name}>
            <NavLink to={route.package(ProjectType.Node, d.name)}>{d.name}</NavLink> ({d.version})
          </div>
        )).value()}
      </div>
    )
  }
}