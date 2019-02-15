import Icon from 'antd/lib/icon'
import Spin from 'antd/lib/spin'
import cx from 'classnames'
import * as React from 'react'

import styles from './index.module.scss'


export type Props = {
  isLoading: boolean
  className?: string
  size?: 'big' | 'normal' | 'small'
}

export default class Loader extends React.Component<Props> {
  getSize() {
    const { size } = this.props

    switch (size) {
      case 'big':
        return 4
      case 'small':
        return 2
      default:
        return 3
    }
  }

  getIndicator() {
    const size = this.getSize()
    const toRem = (value: number) => `${value}rem`
    const style = {
      fontSize: toRem(size),
      width: toRem(size),
      height: toRem(size),
      marginTop: toRem(-size / 2),
      marginLeft: toRem(-size / 2),
    }

    return <Icon type="sync" style={style} spin />
  }

  render() {
    const { className, isLoading } = this.props

    const cls = cx(styles.loader, className)
    const children = React.Children.count(this.props.children) ? this.props.children : <span />

    if (!isLoading)
      return (
        <div className={cls}>
          {children}
        </div>
      )

    return (
      <Spin className={cls} wrapperClassName={cls} indicator={this.getIndicator()}>
        {children}
      </Spin>
    )
  }
}