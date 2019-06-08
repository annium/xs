import Icon from 'antd/lib/icon'
import Spin from 'antd/lib/spin'
import cx from 'classnames'
import React, { Children, ReactNode } from 'react'

import styles from './index.module.scss'

export type Props = {
  isLoading: boolean
  className?: string
  size?: 'big' | 'normal' | 'small'
} & { children?: ReactNode }

export const Loader = ({ isLoading, className, size, children }: Props) => {
  const cls = cx(styles.loader, className)
  const childrenResult = Children.count(children) ? children : <span />

  if (!isLoading) return <div className={cls}>{childrenResult}</div>

  return (
    <Spin className={cls} wrapperClassName={cls} indicator={getIndicator(size)}>
      {childrenResult}
    </Spin>
  )
}

function getIndicator(size: Props['size']) {
  const sizeValue = getSize(size)
  const toRem = (value: number) => `${value}rem`
  const style = {
    fontSize: toRem(sizeValue),
    height: toRem(sizeValue),
    marginLeft: toRem(-sizeValue / 2),
    marginTop: toRem(-sizeValue / 2),
    width: toRem(sizeValue),
  }

  return <Icon type="sync" style={style} spin={true} />
}

function getSize(size: Props['size']) {
  switch (size) {
    case 'big':
      return 4
    case 'small':
      return 2
    default:
      return 3
  }
}
