import { openDB, type IDBPDatabase } from 'idb'
import type { ExpenseRequest } from '@/features/expenses/types/expenses.type'
import { addExpense } from '@/features/expenses/services/expensesApi.service'

const DB_NAME = 'expense-manager'
const STORE_NAME = 'offline-expense-queue'

export type QueueItem = {
  id: string
  payload: ExpenseRequest
  createdAt: string
}

export type DrainResult = { ok: number; failed: number }

let dbPromise: Promise<IDBPDatabase> | null = null

function getDb(): Promise<IDBPDatabase> {
  if (!dbPromise) {
    dbPromise = openDB(DB_NAME, 1, {
      upgrade(db) {
        if (!db.objectStoreNames.contains(STORE_NAME)) {
          db.createObjectStore(STORE_NAME, { keyPath: 'id' })
        }
      },
    })
  }
  return dbPromise
}

export function useOfflineQueue() {
  async function enqueue(payload: ExpenseRequest): Promise<string> {
    const db = await getDb()
    const id = crypto.randomUUID()
    const item: QueueItem = { id, payload, createdAt: new Date().toISOString() }
    await db.put(STORE_NAME, item)
    return id
  }

  async function drain(): Promise<DrainResult> {
    const db = await getDb()
    const items: QueueItem[] = await db.getAll(STORE_NAME)
    let ok = 0
    let failed = 0
    for (const item of items) {
      const res = await addExpense(item.payload)
      if (res.ok) {
        await db.delete(STORE_NAME, item.id)
        ok++
      } else {
        failed++
      }
    }
    return { ok, failed }
  }

  async function getAll(): Promise<QueueItem[]> {
    const db = await getDb()
    return db.getAll(STORE_NAME)
  }

  async function clear(): Promise<void> {
    const db = await getDb()
    await db.clear(STORE_NAME)
  }

  return { enqueue, drain, getAll, clear }
}
