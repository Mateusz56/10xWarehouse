import { createApp } from 'vue'
import { pinia } from '@/stores'

export default (app: any) => {
  app.use(pinia)
}
