import Avatar from 'antd/lib/avatar'
import Button from 'antd/lib/button'
import Dropdown from 'antd/lib/dropdown'
import Menu, { ClickParam } from 'antd/lib/menu'
import React from 'react'

import { ProjectType } from '../../models/view/ProjectType'

import styles from './index.module.scss'


type Props = {
  type: ProjectType
  onSelect(type: ProjectType): void;
}

const ProjectTypes: (keyof typeof ProjectType)[] = Object.keys(ProjectType)
  .filter((key: string | number) => typeof key === 'string')
  .map(key => key as keyof typeof ProjectType)

export const ProjectTypeSelect = ({ type, onSelect }: Props) => (
  <Dropdown overlay={renderMenu(onSelect)} trigger={['click']}>
    <Button>{renderItem(type)}</Button>
  </Dropdown>
)

const renderMenu = (onSelect: Props['onSelect']) => (
  <Menu onClick={handleMenuClick(onSelect)}>
    {ProjectTypes.map(key => (
      <Menu.Item key={key}>
        {renderItem(ProjectType[key])}
      </Menu.Item>
    ))}
  </Menu>
)

const renderItem = (type: ProjectType) => (
  <div className={styles.item}>
    {type ? <Avatar src={`/icons/${type}.svg`} size="small" /> : <Avatar icon="question" size={24} />}
    <span className={styles.label}>
      {ProjectTypes.find(key => ProjectType[key] === type)}
    </span>
  </div>
)

const handleMenuClick = (onSelect: Props['onSelect']) => (param: ClickParam) =>
  onSelect(ProjectType[param.key as keyof typeof ProjectType])

