import { Injectable } from '@angular/core';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import * as xlsx from 'xlsx';
import { Clase } from '../interfaces/clase.interface';

@Injectable({ providedIn: 'root' })
export class ExportService {

  exportarClasesAlumnoPDF(alumnoNombre: string, materia: string, clases: Clase[]) {
    const doc = new jsPDF();
    doc.setFontSize(16);
    doc.text(`Historial de Clases: ${alumnoNombre}`, 14, 20);
    doc.setFontSize(12);
    doc.text(`Materia: ${materia}`, 14, 28);
    
    const realizadas = clases.filter(c => c.estado === 'Realizada');
    const totalRealizadas = realizadas.reduce((sum, c) => sum + (c.precio || 0), 0);
    doc.text(`Total ingresos generados (Realizadas): $${totalRealizadas}`, 14, 34);

    const data = clases.map(c => [
      this.formatFecha(c.fecha),
      c.horaInicio,
      `${c.duracionMinutos} min`,
      c.estado,
      `$${c.precio || 0}`
    ]);

    autoTable(doc, {
      startY: 40,
      head: [['Fecha', 'Hora', 'Duración', 'Estado', 'Precio']],
      body: data,
      theme: 'striped',
      headStyles: { fillColor: [59, 130, 246] }
    });

    const nombre = alumnoNombre.replace(/\s+/g, '_').toLowerCase();
    doc.save(`historial-clases-${nombre}.pdf`);
  }

  exportarDashboardPDF(data: any, currentMonth: string) {
    const doc = new jsPDF();
    doc.setFontSize(18);
    doc.text('Reporte Mensual Tracklass', 14, 20);
    doc.setFontSize(12);
    doc.text(`Período: ${currentMonth}`, 14, 28);

    doc.setFontSize(11);
    doc.text(`Alumnos Activos: ${data.alumnosActivos}`, 14, 40);
    doc.text(`Clases Realizadas: ${data.clasesRealizadasMes}`, 14, 46);
    doc.text(`Ingresos Reales: $${data.ingresosMes}`, 14, 52);
    doc.text(`Ingresos Estimados: $${data.ingresosEstimadosMes}`, 14, 58);

    const mesStr = currentMonth.replace(/\s+/g, '_').toLowerCase();
    doc.save(`reporte-${mesStr}.pdf`);
  }

  exportarAgendaExcel(clases: any[]) {
    const data = clases.map(c => ({
      'Alumno': c.alumnoNombre,
      'Materia': c.materia,
      'Fecha': this.formatFecha(c.fecha),
      'Hora Inicio': c.horaInicio,
      'Duración (min)': c.duracionMinutos,
      'Estado': c.estado,
      'Precio ($)': c.precio || 0
    }));

    const ws = xlsx.utils.json_to_sheet(data);
    const wb = xlsx.utils.book_new();
    xlsx.utils.book_append_sheet(wb, ws, "Agenda");

    const fechaStr = new Date().toISOString().split('T')[0];
    xlsx.writeFile(wb, `agenda-tracklass-${fechaStr}.xlsx`);
  }

  private formatFecha(fecha: string): string {
    const d = new Date(fecha);
    const offset = d.getTimezoneOffset();
    const local = new Date(d.getTime() + (offset * 60 * 1000));
    
    return local.toLocaleDateString('es-UY', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    });
  }
}
