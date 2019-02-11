import Table, { ColumnProps } from 'antd/lib/table'
import cx from 'classnames'
import React from 'react'
import { NavLink } from 'react-router-dom'

import MetaPackage from '../../models/view/MetaPackage'
import Package from '../../models/view/Package'
import route from '../../utils/route'

import styles from './index.module.scss'


type Props = {
  metaPackage: MetaPackage
  packages: Package[]
}

export default class PackageVersions extends React.PureComponent<Props> {
  render() {
    const { metaPackage, packages } = this.props

    return (
      <>
        <div className={styles.header}>Version History</div>
        <Table<Package>
          rowKey="version"
          columns={this.getColumns(metaPackage)}
          dataSource={packages}
          pagination={false}
          size="small"
          onRow={this.getRowProps} />
      </>
    )
  }

  private getColumns(metaPackage: MetaPackage): ColumnProps<Package>[] {
    return ([
      {
        title: 'Version',
        dataIndex: 'version',
        key: 'version',
        className: styles.link,
        render: (_, record) => (
          <NavLink to={route.package(metaPackage.type, record.name, record.version)}>{record.version}</NavLink>
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
    const { metaPackage } = this.props

    return {
      className: cx({
        [styles.current]: record.version === metaPackage.version,
      }),
    }
  }
}