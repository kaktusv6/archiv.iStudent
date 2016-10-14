class ArifmCode:

	table = []
	segm = {}
	rez = 0.
	destr = ""
	st = ""
	
	def readF(self):
		f = open("input.txt")
		st = f.read()
		f.close()
		return(st)
	

	def clear(self):
		o = open('outputArCod.txt', 'w')
		o.close()

	def findInTable(self, sym):
		for i in range(len(self.table)):
			if sym == self.table[i][0]:
				return self.table[i][1]

	def findInterval(self):
		j = 0
		for i in self.table:
			if  (self.rez > j) and (self.rez < (i[1]+j)):
				# print(i[0])
				return i[0]
			j = j + i[1]

	def writ(self, sym):
		f = open("outputArCod.txt", 'a')
		f.write(str(sym))
		f.write('\n')
		f.close()



	def newSegm(self, lowO, HighO, i):
		i += 1
		highR = self.segm[self.st[i]]
		lowR = self.segm[self.st[i]] - self.findInTable(self.st[i])

		low = lowO + (HighO - lowO) * lowR
		high = lowO + (HighO - lowO) * highR
		# print(low)
		# print(high)

		if i==len(self.st)-1:
			self.rez = low
			return()
		else:
			self.newSegm(low, high, i)



	def primaryTable(self):

		for i in range(len(self.st)):
			if self.st[i] not in self.table:
				self.table.append((self.st[i]))

		for i in range(len(self.table)):
			sym = self.table.pop(i)
			self.table.insert(i, (sym, (self.st.count(sym)/len(self.st) ) ) )

		self.table.sort(key = lambda x: x[1], reverse = True)

	def constructSegm(self):
		count = 0
		for i in range (len(self.table)):
			count += self.table[i][1]
			other = {self.table[i][0]: count}
			self.segm.update(other)
		 # segm = {self.table[i][0]: count += self.table[i][1] for i in self.table }

	def compress(self):
		self.st = self.readF()
		self.primaryTable()
		self.constructSegm()

		for i in range(len(self.table)):
			print (self.table[i])
			print (self.segm[self.table[i][0]])


		high = self.segm[self.st[0]]
		low = self.segm[self.st[0]] - self.findInTable(self.st[0])
		self.newSegm(low, high, 0)
		self.writ(self.rez)
		
 	
	def decompressor(self):
		
		sym = self.findInterval()
		# print(self.destr)
		self.destr = self.destr + sym

		highR = self.segm[sym]
		lowR = self.segm[sym] - self.findInTable(sym)

		self.rez = (self.rez - lowR)/(highR - lowR)


		if len(self.destr) == len(self.st):
			self.writ(self.destr)
			return()
		else:
			self.decompressor()





code = ArifmCode()
code.clear()
code.compress()
code.decompressor()
# code.findInterval()
