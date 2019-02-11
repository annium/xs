import Table, { ColumnProps } from 'antd/lib/table'
import cx from 'classnames'
import React from 'react'
import { NavLink } from 'react-router-dom'

import Package from '../../models/view/Package'
import { ProjectType } from '../../models/view/ProjectType'
import route from '../../utils/route'

import styles from './index.module.scss'


type Props = {
  type: ProjectType
  pkg: Package
  packages: Package[]
}

export default class PackageVersions extends React.PureComponent<Props> {
  render() {
    const { type, packages } = this.props

    return (
      <>
        <div className={styles.header}>Version History</div>
        <Table<Package>
          rowKey="version"
          columns={this.getColumns(type)}
          dataSource={packages}
          pagination={false}
          size="small"
          onRow={this.getRowProps} />
      </>
    )
  }

  private getColumns(type: ProjectType): ColumnProps<Package>[] {
    return ([
      {
        title: 'Version',
        dataIndex: 'version',
        key: 'version',
        className: styles.link,
        render: (_, record) => (
          <NavLink to={route.package(type, record.name, record.version)}>{record.version}</NavLink>
        ),
      },
      {
        title: 'Downloads',
        dataIndex: 'downloads',
        key: 'downloads',
        render: (_, record) => record.downloads.toLocaleString(),
      },
      {
        title: 'Published',
        dataIndex: 'published',
        key: 'published',
        render: (_, record) => record.published.fromNow(),
      },
    ])
  }

  private getRowProps = (record: Package) => {
    const { pkg } = this.props

    return {
      className: cx({
        [styles.current]: record.version === pkg.version,
      }),
    }
  }
}