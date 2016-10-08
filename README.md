# archiv.iStudent

Для запуска архиватора перейдите в папку `bin/Debug`
и через консоль введите команду:

`$ ConsoleApplicationArchivy <mode> [Files, ]`

Где `<mode>` режим архиватора:

1. `--com` архиваирование файлов
2. `--uncom` деархиваирование файлов

`[Files, ]` имена файлов которые вы хотите архивировать или деархивировать:

Пример `ConsoleApplicationArchivy --com input.txt output.txt`

В данном пример будуд архивироваться два файла `input.txt` и `output.txt`.
По окончанию программы в той же директории будудт два файла `input.txt.sz` и `output.txt.sz`.