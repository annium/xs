import List from 'antd/lib/list'
import React from 'react'

import { MetaPackage } from '../../models/view/MetaPackage'
import { PackageItem } from '../PackageItem'


import styles from './index.module.scss'

type Props = {
  packages: MetaPackage[]
}

export const PackageList = ({ packages }: Props) => (
  <>
    {renderHeader(packages)}
    <div className={styles.container}>
      <List
        itemLayout="vertical"
        dataSource={packages}
        renderItem={renderPackage}
      />
    </div>
  </>
)

const renderHeader = (packages: MetaPackage[]) => (
  <h2 className={styles.header}>Total {packages.length} packages:</h2>
)

const renderPackage = (pkg: MetaPackage, index: number) => (
  <PackageItem key={index} pkg={pkg} />
)
