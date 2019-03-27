import React, { ReactNode } from 'react'

import styles from './index.module.scss'


export const Root = ({ children }: { children?: ReactNode }) => (
  <div className={styles.root}>
    {children}
  </div>
)
