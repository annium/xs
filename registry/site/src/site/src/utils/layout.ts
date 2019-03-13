import { ColSize } from 'antd/lib/grid'

export function getCenteredLayout(
  xs: number,
  sm: number,
  md: number,
  lg: number,
  xl: number,
): { [key: string]: ColSize } {
  return {
    lg: { span: lg, offset: (24 - lg) / 2 },
    md: { span: md, offset: (24 - md) / 2 },
    sm: { span: sm, offset: (24 - sm) / 2 },
    xl: { span: xl, offset: (24 - xl) / 2 },
    xs: { span: xs, offset: (24 - xs) / 2 },
  }
}

export const gutter = { xs: 10, sm: 12, md: 14, lg: 14, xl: 14 }
