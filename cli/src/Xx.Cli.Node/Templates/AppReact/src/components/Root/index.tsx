import React, { ReactNode } from 'react'

import { useStyles } from './styles'


export const Root = ({ children }: { children?: ReactNode }) => {
  const styles = useStyles()

  return (
    <div className={styles.root}>
      {children}
    </div>
  )
}
