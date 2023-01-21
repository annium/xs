import React, { ReactNode } from 'react'

import styles from './index.module.scss'
import { Menu } from './Menu'


export const Root = ({ children }: { children?: ReactNode }) => (
  <div className={styles.root}>
    <div className={styles.content}>
      {children}
    </div>
    <Menu />
  </div>
)
