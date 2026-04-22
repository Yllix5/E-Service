package main

import (
	"bufio"
	"fmt"
	"math/rand"
	"os"
	"time"
)

const (
	yearCode          = 26
	contractFile      = "used_contracts.txt"
	backupFile        = "used_backups.txt"
	totalContractCombos = 90000
	totalBackupCombos   = 900000 
)

func loadUsedCodes(filename string) map[string]bool {
	file, err := os.Open(filename)
	if err != nil {
		return make(map[string]bool)
	}
	defer file.Close()

	used := make(map[string]bool)
	scanner := bufio.NewScanner(file)
	for scanner.Scan() {
		used[scanner.Text()] = true
	}
	return used
}

func saveCode(filename, code string) {
	file, err := os.OpenFile(filename, os.O_APPEND|os.O_CREATE|os.O_WRONLY, 0644)
	if err != nil {
		fmt.Println("Error writing code:", err)
		return
	}
	defer file.Close()
	file.WriteString(code + "\n")
}

func generateUniqueContract(used map[string]bool) (string, bool) {
	rand.Seed(time.Now().UnixNano())

	if len(used) >= totalContractCombos {
		return "", false
	}

	for {
		num := rand.Intn(90000) + 10000
		code := fmt.Sprintf("RE-%05d/%d", num, yearCode)
		if !used[code] {
			return code, true
		}
	}
}

func generateUniqueBackup(used map[string]bool) (string, bool) {
	rand.Seed(time.Now().UnixNano())

	if len(used) >= totalBackupCombos {
		return "", false
	}

	for {
		num := rand.Intn(900000) + 100000 // 6-digit number
		code := fmt.Sprintf("%06d", num)
		if !used[code] {
			return code, true
		}
	}
}

func main() {
	usedContracts := loadUsedCodes(contractFile)
	usedBackups := loadUsedCodes(backupFile)

	contractCode, ok1 := generateUniqueContract(usedContracts)
	backupCode, ok2 := generateUniqueBackup(usedBackups)

	if ok1 {
		saveCode(contractFile, contractCode)
		fmt.Println("Numri i kontrates se gjeneruar te studentit:", contractCode)
		fmt.Printf("Edhe kaq kontrata mund te gjenerohen: %d\n", totalContractCombos-len(usedContracts)-1)
	} else {
		fmt.Println("Te gjitha kodet e kontratave jane gjeneruar.")
	}

	if ok2 {
		saveCode(backupFile, backupCode)
		fmt.Println("Numri i gjeneruar i Backup code:", backupCode)
		fmt.Printf("Edhe kaq kombinime kane mbetur: %d\n", totalBackupCombos-len(usedBackups)-1)
	} else {
		fmt.Println("Te gjitha backup kodet jane gjeneruar.")
	}
}
