import Table, { ColumnProps } from 'antd/lib/table'
import cx from 'classnames'
import React from 'react'
import { NavLink } from 'react-router-dom'

import { Package } from '../../models/view/Package'
import { ProjectType } from '../../models/view/ProjectType'
import * as route from '../../utils/route'

import styles from './index.module.scss'


type Props = {
  type: ProjectType
  pkg: Package
  packages: Package[]
}

export const PackageVersions = ({ type, pkg, packages }: Props) => (
  <>
    <div className={styles.header}>Version History</div>
    <Table<Package>
      rowKey="version"
      columns={getColumns(type)}
      dataSource={packages}
      pagination={false}
      size="small"
      onRow={getRowProps(pkg)}
    />
  </>
)

const getColumns = (type: ProjectType): ColumnProps<Package>[] => ([
  {
    title: 'Version',
    key: 'version',
    dataIndex: 'version',
    render: (_, record) => (
      <NavLink to={route.pkg(type, record.name, record.version)}>{record.version}</NavLink>
    ),
    className: styles.link,
  },
  {
    title: 'Downloads',
    key: 'downloads',
    dataIndex: 'downloads',
    render: (_, record) => record.downloads.toLocaleString(),
  },
  {
    title: 'Published',
    key: 'published',
    dataIndex: 'published',
    render: (_, record) => record.published.fromNow(),
  },
])


const getRowProps = (pkg: Package) => (record: Package) => ({
  className: cx({
    [styles.current]: record.version === pkg.version,
  }),
})
